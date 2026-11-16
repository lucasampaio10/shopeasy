import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { FlatList, StyleSheet, Text, View } from 'react-native';
import { useCart } from '../../application/CartContext';
import { colors, spacing, typography } from '../../theme/tokens';
import { CartItemRow } from '../components/CartItemRow';
import { EmptyState } from '../components/EmptyState';
import { PrimaryButton } from '../components/PrimaryButton';
import { formatMoney } from '../format';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Cart'>;

export function CartScreen({ navigation }: Props) {
  const { cart, total, currency, setQuantity, remove } = useCart();

  if (cart.lines.length === 0) {
    return (
      <View style={styles.screen}>
        <EmptyState title="Seu carrinho está vazio" message="Volte ao catálogo para escolher alguma coisa." />
        <View style={styles.footer}>
          <PrimaryButton label="Ver catálogo" onPress={() => navigation.navigate('Products')} />
        </View>
      </View>
    );
  }

  return (
    <View style={styles.screen}>
      <FlatList
        data={cart.lines}
        keyExtractor={(line) => line.productId}
        renderItem={({ item }) => (
          <CartItemRow
            line={item}
            onChangeQuantity={(quantity) => setQuantity(item.productId, quantity)}
            onRemove={() => remove(item.productId)}
          />
        )}
      />

      <View style={styles.footer}>
        <View style={styles.totalRow}>
          <Text style={styles.totalLabel}>Total</Text>
          <Text style={styles.totalValue}>{formatMoney(total, currency)}</Text>
        </View>
        <PrimaryButton label="Finalizar compra" onPress={() => navigation.navigate('Checkout')} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  footer: { padding: spacing.lg, borderTopWidth: 1, borderTopColor: colors.border, backgroundColor: colors.surface, gap: spacing.md },
  totalRow: { flexDirection: 'row', justifyContent: 'space-between' },
  totalLabel: { ...typography.subtitle, color: colors.textSecondary },
  totalValue: { ...typography.title, color: colors.textPrimary },
});
