import { StyleSheet, Text, View } from 'react-native';
import { OrderStatus } from '../../domain/entities';
import { colors, radius, typography } from '../../theme/tokens';
import { statusLabel } from '../format';

const PALETTE: Record<OrderStatus, { fg: string; bg: string }> = {
  Pending: { fg: colors.textSecondary, bg: colors.surfaceMuted },
  AwaitingPayment: { fg: colors.warning, bg: colors.warningSoft },
  Paid: { fg: colors.success, bg: colors.successSoft },
  Cancelled: { fg: colors.danger, bg: colors.dangerSoft },
};

export function StatusBadge({ status }: { status: OrderStatus }) {
  const tone = PALETTE[status];

  return (
    <View style={[styles.badge, { backgroundColor: tone.bg }]}>
      <Text style={[styles.label, { color: tone.fg }]}>{statusLabel(status)}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: { alignSelf: 'flex-start', paddingVertical: 6, paddingHorizontal: 12, borderRadius: radius.pill },
  label: { ...typography.label, textTransform: 'uppercase' },
});
