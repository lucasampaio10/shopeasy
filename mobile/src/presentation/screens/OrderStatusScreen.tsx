import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useOrderTracking } from '../../application/useOrderTracking';
import { colors, radius, spacing, typography } from '../../theme/tokens';
import { PrimaryButton } from '../components/PrimaryButton';
import { StatusBadge } from '../components/StatusBadge';
import { formatMoney } from '../format';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'OrderStatus'>;

const STEP_MESSAGE: Record<string, string> = {
  Pending: 'Recebemos seu pedido e estamos conferindo o estoque.',
  AwaitingPayment: 'Estoque reservado — agora é só aguardar a confirmação do pagamento.',
  Paid: 'Pagamento aprovado! Seu pedido está confirmado.',
  Cancelled: 'Este pedido não pôde ser concluído.',
};

export function OrderStatusScreen({ route, navigation }: Props) {
  const { order, failedToReach, isSettled } = useOrderTracking(route.params.orderId);

  return (
    <View style={styles.screen}>
      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.eyebrow}>Pedido</Text>
        <Text style={styles.orderId}>#{route.params.orderId.slice(0, 8).toUpperCase()}</Text>

        {!order ? (
          <View style={styles.loadingRow}>
            <ActivityIndicator color={colors.accent} />
            <Text style={styles.loadingText}>Carregando o pedido…</Text>
          </View>
        ) : (
          <>
            <StatusBadge status={order.status} />
            <Text style={styles.statusMessage}>{STEP_MESSAGE[order.status]}</Text>

            {order.status === 'Cancelled' && order.cancellationReason && (
              <Text style={styles.cancellationReason}>Motivo: {order.cancellationReason}</Text>
            )}

            {!isSettled && (
              <View style={styles.progressRow}>
                <ActivityIndicator color={colors.accent} size="small" />
                <Text style={styles.progressText}>Acompanhando em tempo real…</Text>
              </View>
            )}

            <View style={styles.card}>
              {order.lines.map((line) => (
                <View key={line.productId} style={styles.lineRow}>
                  <Text style={styles.lineName} numberOfLines={1}>
                    {line.quantity}× {line.productName}
                  </Text>
                  <Text style={styles.lineAmount}>{formatMoney(line.unitPrice * line.quantity, order.currency)}</Text>
                </View>
              ))}
              <View style={[styles.lineRow, styles.totalRow]}>
                <Text style={styles.totalLabel}>Total</Text>
                <Text style={styles.totalValue}>{formatMoney(order.total, order.currency)}</Text>
              </View>
            </View>
          </>
        )}

        {failedToReach && <Text style={styles.warning}>Perdemos a conexão por um instante — tentando de novo…</Text>}
      </ScrollView>

      <View style={styles.footer}>
        <PrimaryButton label="Voltar à loja" onPress={() => navigation.reset({ index: 0, routes: [{ name: 'Products' }] })} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg, gap: spacing.sm },
  eyebrow: { ...typography.label, color: colors.textMuted },
  orderId: { ...typography.display, color: colors.textPrimary, marginBottom: spacing.sm },
  loadingRow: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm, marginTop: spacing.lg },
  loadingText: { ...typography.body, color: colors.textSecondary },
  statusMessage: { ...typography.body, color: colors.textSecondary, marginTop: spacing.md, lineHeight: 21 },
  cancellationReason: { ...typography.caption, color: colors.danger, marginTop: spacing.sm },
  progressRow: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm, marginTop: spacing.md },
  progressText: { ...typography.caption, color: colors.textMuted },
  card: {
    marginTop: spacing.xl,
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.lg,
  },
  lineRow: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: spacing.xs, gap: spacing.sm },
  lineName: { ...typography.body, color: colors.textPrimary, flex: 1 },
  lineAmount: { ...typography.body, color: colors.textSecondary },
  totalRow: { borderTopWidth: 1, borderTopColor: colors.border, marginTop: spacing.sm, paddingTop: spacing.md },
  totalLabel: { ...typography.subtitle, color: colors.textPrimary },
  totalValue: { ...typography.subtitle, color: colors.textPrimary },
  warning: { ...typography.caption, color: colors.warning, marginTop: spacing.lg },
  footer: { padding: spacing.lg, borderTopWidth: 1, borderTopColor: colors.border, backgroundColor: colors.surface },
});
