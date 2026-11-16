import { ActivityIndicator, Pressable, StyleSheet, Text } from 'react-native';
import { colors, radius, spacing, typography } from '../../theme/tokens';

type PrimaryButtonProps = {
  label: string;
  onPress: () => void;
  disabled?: boolean;
  loading?: boolean;
  variant?: 'solid' | 'outline';
};

export function PrimaryButton({ label, onPress, disabled, loading, variant = 'solid' }: PrimaryButtonProps) {
  const isDisabled = disabled || loading;
  const outline = variant === 'outline';

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={{ disabled: isDisabled }}
      onPress={onPress}
      disabled={isDisabled}
      style={({ pressed }) => [
        styles.base,
        outline ? styles.outline : styles.solid,
        isDisabled && styles.disabled,
        pressed && !isDisabled && (outline ? styles.outlinePressed : styles.solidPressed),
      ]}
    >
      {loading ? (
        <ActivityIndicator color={outline ? colors.accent : colors.textOnAccent} />
      ) : (
        <Text style={[styles.label, outline && styles.labelOutline]}>{label}</Text>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  base: {
    height: 52,
    borderRadius: radius.md,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: spacing.lg,
  },
  solid: { backgroundColor: colors.accent },
  solidPressed: { backgroundColor: colors.accentPressed },
  outline: { backgroundColor: 'transparent', borderWidth: 1.5, borderColor: colors.accent },
  outlinePressed: { backgroundColor: colors.accentSoft },
  disabled: { opacity: 0.45 },
  label: { ...typography.subtitle, color: colors.textOnAccent },
  labelOutline: { color: colors.accent },
});
