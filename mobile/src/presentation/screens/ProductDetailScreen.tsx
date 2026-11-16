import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useCart } from '../../application/CartContext';
import { useProduct } from '../../application/useProduct';
import { colors, radius, spacing, typography } from '../../theme/tokens';
import { EmptyState } from '../components/EmptyState';
import { PrimaryButton } from '../components/PrimaryButton';
import { QuantityStepper } from '../components/QuantityStepper';
import { formatMoney } from '../format';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'ProductDetail'>;

export function ProductDetailScreen({ route, navigation }: Props) {
  const { product, status } = useProduct(route.params.productId);
  const { add } = useCart();
  const [quantity, setQuantity] = useState(1);
  const [justAdded, setJustAdded] = useState(false);

  if (status === 'loading') {
    return (
      <View style={styles.centered}>
        <ActivityIndicator color={colors.accent} />
      </View>
    );
  }

  if (status === 'error' || !product) {
    return <EmptyState title="Produto não encontrado" message="Ele pode ter sido removido do catálogo." />;
  }

  const soldOut = product.availableStock === 0;

  function handleAdd() {
    if (!product) return;
    add(product, quantity);
    setJustAdded(true);
    setTimeout(() => setJustAdded(false), 1800);
  }

  return (
    <View style={styles.screen}>
      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.hero}>
          <Text style={styles.heroInitial}>{product.name.charAt(0)}</Text>
        </View>

        <Text style={styles.sku}>SKU {product.sku}</Text>
        <Text style={styles.name}>{product.name}</Text>
        <Text style={styles.price}>{formatMoney(product.price.amount, product.price.currency)}</Text>

        <Text style={styles.description}>{product.description}</Text>

        <Text style={soldOut ? styles.soldOut : styles.stock}>
          {soldOut ? 'Sem estoque no momento' : `${product.availableStock} em estoque`}
        </Text>

        {!soldOut && (
          <View style={styles.quantityRow}>
            <Text style={styles.quantityLabel}>Quantidade</Text>
            <QuantityStepper quantity={quantity} onChange={setQuantity} max={product.availableStock} />
          </View>
        )}
      </ScrollView>

      <View style={styles.footer}>
        {justAdded && <Text style={styles.confirmation}>Adicionado ao carrinho ✓</Text>}
        {soldOut ? (
          <PrimaryButton label="Avise-me quando chegar" onPress={() => {}} disabled />
        ) : (
          <View style={styles.footerButtons}>
            <PrimaryButton label="Adicionar ao carrinho" onPress={handleAdd} variant="outline" />
            <PrimaryButton
              label="Comprar agora"
              onPress={() => {
                handleAdd();
                navigation.navigate('Cart');
              }}
            />
          </View>
        )}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  centered: { flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: colors.background },
  content: { padding: spacing.lg, gap: spacing.sm },
  hero: {
    height: 180,
    borderRadius: radius.lg,
    backgroundColor: colors.accentSoft,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
  },
  heroInitial: { fontSize: 56, fontWeight: '800', color: colors.accent },
  sku: { ...typography.label, color: colors.textMuted },
  name: { ...typography.display, color: colors.textPrimary },
  price: { ...typography.title, color: colors.accent },
  description: { ...typography.body, color: colors.textSecondary, marginTop: spacing.sm, lineHeight: 22 },
  stock: { ...typography.caption, color: colors.success, marginTop: spacing.md, fontWeight: '600' },
  soldOut: { ...typography.caption, color: colors.danger, marginTop: spacing.md, fontWeight: '600' },
  quantityRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: spacing.lg },
  quantityLabel: { ...typography.subtitle, color: colors.textPrimary },
  footer: {
    padding: spacing.lg,
    borderTopWidth: 1,
    borderTopColor: colors.border,
    backgroundColor: colors.surface,
    gap: spacing.sm,
  },
  footerButtons: { flexDirection: 'row', gap: spacing.sm },
  confirmation: { ...typography.caption, color: colors.success, fontWeight: '600', textAlign: 'center' },
});
