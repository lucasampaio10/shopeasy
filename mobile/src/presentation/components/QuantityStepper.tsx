import { Pressable, StyleSheet, Text, View } from 'react-native';
import { colors, radius, typography } from '../../theme/tokens';

type QuantityStepperProps = {
  quantity: number;
  onChange: (quantity: number) => void;
  max?: number;
};

export function QuantityStepper({ quantity, onChange, max }: QuantityStepperProps) {
  const canIncrease = max === undefined || quantity < max;

  return (
    <View style={styles.row}>
      <StepButton symbol="–" onPress={() => onChange(quantity - 1)} label="Diminuir quantidade" />
      <Text style={styles.value}>{quantity}</Text>
      <StepButton symbol="+" onPress={() => onChange(quantity + 1)} disabled={!canIncrease} label="Aumentar quantidade" />
    </View>
  );
}

function StepButton({ symbol, onPress, disabled, label }: { symbol: string; onPress: () => void; disabled?: boolean; label: string }) {
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      disabled={disabled}
      onPress={onPress}
      style={({ pressed }) => [styles.button, disabled && styles.buttonDisabled, pressed && !disabled && styles.buttonPressed]}
    >
      <Text style={styles.symbol}>{symbol}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  row: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  button: {
    width: 30,
    height: 30,
    borderRadius: radius.sm,
    backgroundColor: colors.surfaceMuted,
    alignItems: 'center',
    justifyContent: 'center',
  },
  buttonPressed: { backgroundColor: colors.border },
  buttonDisabled: { opacity: 0.4 },
  symbol: { ...typography.subtitle, color: colors.textPrimary, lineHeight: 18 },
  value: { ...typography.subtitle, color: colors.textPrimary, minWidth: 26, textAlign: 'center' },
});
