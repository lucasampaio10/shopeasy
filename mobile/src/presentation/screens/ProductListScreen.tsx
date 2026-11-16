import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useState } from 'react';
import { ActivityIndicator, FlatList, StyleSheet, Text, TextInput, View } from 'react-native';
import { useCart } from '../../application/CartContext';
import { useProducts } from '../../application/useProducts';
import { colors, spacing, typography } from '../../theme/tokens';
import { CartBadge } from '../components/CartBadge';
import { EmptyState } from '../components/EmptyState';
import { ProductCard } from '../components/ProductCard';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Products'>;

export function ProductListScreen({ navigation }: Props) {
  const [search, setSearch] = useState('');
  const { products, status } = useProducts(search);
  const { itemCount } = useCart();

  return (
    <View style={styles.screen}>
      <View style={styles.header}>
        <View>
          <Text style={styles.brand}>ShopEasy</Text>
          <Text style={styles.tagline}>Eletrônicos e acessórios pra sua mesa de trabalho</Text>
        </View>
        <CartBadge count={itemCount} onPress={() => navigation.navigate('Cart')} />
      </View>

      <TextInput
        value={search}
        onChangeText={setSearch}
        placeholder="Buscar produto"
        placeholderTextColor={colors.textMuted}
        style={styles.search}
        autoCorrect={false}
      />

      {status === 'loading' && (
        <View style={styles.centered}>
          <ActivityIndicator color={colors.accent} />
        </View>
      )}

      {status === 'error' && (
        <EmptyState title="Não deu pra carregar a loja" message="Confira se a API do ShopEasy está no ar e tente de novo." />
      )}

      {status === 'ready' && products.length === 0 && (
        <EmptyState title="Nada por aqui" message="Nenhum produto encontrado para essa busca." />
      )}

      {status === 'ready' && products.length > 0 && (
        <FlatList
          data={products}
          keyExtractor={(product) => product.id}
          renderItem={({ item }) => (
            <ProductCard product={item} onPress={() => navigation.navigate('ProductDetail', { productId: item.id })} />
          )}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: spacing.lg,
    paddingTop: spacing.lg,
    paddingBottom: spacing.sm,
  },
  brand: { ...typography.display, color: colors.textPrimary },
  tagline: { ...typography.caption, color: colors.textSecondary, marginTop: 2, maxWidth: 260 },
  search: {
    marginHorizontal: spacing.lg,
    marginBottom: spacing.md,
    height: 44,
    borderRadius: 10,
    paddingHorizontal: spacing.md,
    backgroundColor: colors.surfaceMuted,
    color: colors.textPrimary,
    ...typography.body,
  },
  centered: { flex: 1, alignItems: 'center', justifyContent: 'center' },
});
