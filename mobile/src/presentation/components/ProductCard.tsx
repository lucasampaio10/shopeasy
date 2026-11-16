import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Product } from '../../domain/entities';
import { colors, radius, spacing, typography } from '../../theme/tokens';
import { formatMoney } from '../format';

const LOW_STOCK_THRESHOLD = 5;

export function ProductCard({ product, onPress }: { product: Product; onPress: () => void }) {
  const soldOut = product.availableStock === 0;

  return (
    <Pressable
      accessibilityRole="button"
      onPress={onPress}
      style={({ pressed }) => [styles.card, pressed && styles.cardPressed]}
    >
      <View style={styles.thumbnail}>
        <Text style={styles.initial}>{product.name.charAt(0)}</Text>
      </View>

      <View style={styles.info}>
        <Text style={styles.name} numberOfLines={1}>
          {product.name}
        </Text>
        <Text style={styles.description} numberOfLines={1}>
          {product.description}
        </Text>
        <StockHint stock={product.availableStock} />
      </View>

      <Text style={[styles.price, soldOut && styles.priceMuted]}>{formatMoney(product.price.amount, product.price.currency)}</Text>
    </Pressable>
  );
}

function StockHint({ stock }: { stock: number }) {
  if (stock === 0) {
    return <Text style={styles.soldOut}>Esgotado</Text>;
  }

  if (stock <= LOW_STOCK_THRESHOLD) {
    return <Text style={styles.lowStock}>Últimas {stock} unidades</Text>;
  }

  return null;
}

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.lg,
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  cardPressed: { backgroundColor: colors.surfaceMuted },
  thumbnail: {
    width: 48,
    height: 48,
    borderRadius: radius.md,
    backgroundColor: colors.accentSoft,
    alignItems: 'center',
    justifyContent: 'center',
  },
  initial: { ...typography.title, color: colors.accent },
  info: { flex: 1, gap: 2 },
  name: { ...typography.subtitle, color: colors.textPrimary },
  description: { ...typography.caption, color: colors.textSecondary },
  soldOut: { ...typography.caption, color: colors.danger, fontWeight: '600' },
  lowStock: { ...typography.caption, color: colors.warning, fontWeight: '600' },
  price: { ...typography.subtitle, color: colors.textPrimary },
  priceMuted: { color: colors.textMuted },
});
