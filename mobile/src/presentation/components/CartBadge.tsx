import { Pressable, StyleSheet, Text, View } from 'react-native';
import { colors, radius, typography } from '../../theme/tokens';

export function CartBadge({ count, onPress }: { count: number; onPress: () => void }) {
  return (
    <Pressable accessibilityRole="button" accessibilityLabel="Abrir carrinho" onPress={onPress} style={styles.touchArea}>
      <Text style={styles.icon}>🛍</Text>
      {count > 0 && (
        <View style={styles.badge}>
          <Text style={styles.badgeLabel}>{count > 9 ? '9+' : count}</Text>
        </View>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  touchArea: { width: 40, height: 40, alignItems: 'center', justifyContent: 'center' },
  icon: { fontSize: 20 },
  badge: {
    position: 'absolute',
    top: 2,
    right: 2,
    minWidth: 17,
    height: 17,
    paddingHorizontal: 3,
    borderRadius: radius.pill,
    backgroundColor: colors.accent,
    alignItems: 'center',
    justifyContent: 'center',
  },
  badgeLabel: { ...typography.label, fontSize: 10, color: colors.textOnAccent },
});
