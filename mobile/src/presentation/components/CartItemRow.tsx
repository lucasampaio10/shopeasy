import { Pressable, StyleSheet, Text, View } from 'react-native';
import { CartLine } from '../../domain/entities';
import { colors, spacing, typography } from '../../theme/tokens';
import { formatMoney } from '../format';
import { QuantityStepper } from './QuantityStepper';

type CartItemRowProps = {
  line: CartLine;
  onChangeQuantity: (quantity: number) => void;
  onRemove: () => void;
};

export function CartItemRow({ line, onChangeQuantity, onRemove }: CartItemRowProps) {
  return (
    <View style={styles.row}>
      <View style={styles.info}>
        <Text style={styles.name} numberOfLines={1}>
          {line.name}
        </Text>
        <Text style={styles.unitPrice}>{formatMoney(line.unitPrice.amount, line.unitPrice.currency)} cada</Text>
        <Pressable accessibilityRole="button" onPress={onRemove} hitSlop={8}>
          <Text style={styles.remove}>Remover</Text>
        </Pressable>
      </View>

      <View style={styles.trailing}>
        <Text style={styles.lineTotal}>{formatMoney(line.unitPrice.amount * line.quantity, line.unitPrice.currency)}</Text>
        <QuantityStepper quantity={line.quantity} onChange={onChangeQuantity} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.lg,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: spacing.md,
  },
  info: { flex: 1, gap: 4 },
  name: { ...typography.subtitle, color: colors.textPrimary },
  unitPrice: { ...typography.caption, color: colors.textSecondary },
  remove: { ...typography.caption, color: colors.danger, fontWeight: '600' },
  trailing: { alignItems: 'flex-end', gap: spacing.sm },
  lineTotal: { ...typography.subtitle, color: colors.textPrimary },
});
