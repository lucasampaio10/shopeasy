import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useState } from 'react';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useCart } from '../../application/CartContext';
import { usePlaceOrder } from '../../application/usePlaceOrder';
import { PAYMENT_METHODS, PaymentToken, validateShippingAddress } from '../../domain/checkout';
import { ShippingAddress } from '../../domain/entities';
import { customerId } from '../../data/session';
import { colors, radius, spacing, typography } from '../../theme/tokens';
import { PrimaryButton } from '../components/PrimaryButton';
import { TextField } from '../components/TextField';
import { formatMoney } from '../format';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Checkout'>;

const EMPTY_ADDRESS: ShippingAddress = { street: '', city: '', state: '', zipCode: '' };

export function CheckoutScreen({ navigation }: Props) {
  const { cart, total, currency, clear } = useCart();
  const { placeOrder, submitting, error } = usePlaceOrder();
  const [address, setAddress] = useState<ShippingAddress>(EMPTY_ADDRESS);
  const [paymentToken, setPaymentToken] = useState<PaymentToken>(PAYMENT_METHODS[0].token);
  const [formErrors, setFormErrors] = useState<string[]>([]);

  async function handleSubmit() {
    const errors = validateShippingAddress(address);
    setFormErrors(errors);
    if (errors.length > 0) return;

    const orderId = await placeOrder({
      customerId,
      shippingAddress: address,
      paymentToken,
      items: cart.lines.map((line) => ({ productId: line.productId, quantity: line.quantity })),
    }).catch(() => null);

    if (orderId) {
      clear();
      navigation.replace('OrderStatus', { orderId });
    }
  }

  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Text style={styles.sectionTitle}>Endereço de entrega</Text>
      <TextField label="Rua e número" value={address.street} onChangeText={(street) => setAddress((a) => ({ ...a, street }))} />
      <View style={styles.row}>
        <View style={styles.flex2}>
          <TextField label="Cidade" value={address.city} onChangeText={(city) => setAddress((a) => ({ ...a, city }))} />
        </View>
        <View style={styles.flex1}>
          <TextField label="UF" value={address.state} autoCapitalize="characters" maxLength={2} onChangeText={(state) => setAddress((a) => ({ ...a, state }))} />
        </View>
      </View>
      <TextField label="CEP" value={address.zipCode} onChangeText={(zipCode) => setAddress((a) => ({ ...a, zipCode }))} keyboardType="numbers-and-punctuation" />

      {formErrors.length > 0 && (
        <View style={styles.errorBox}>
          {formErrors.map((message) => (
            <Text key={message} style={styles.errorText}>
              • {message}
            </Text>
          ))}
        </View>
      )}

      <Text style={styles.sectionTitle}>Pagamento</Text>
      {PAYMENT_METHODS.map((method) => (
        <Pressable
          key={method.token}
          accessibilityRole="radio"
          accessibilityState={{ checked: paymentToken === method.token }}
          onPress={() => setPaymentToken(method.token)}
          style={[styles.paymentOption, paymentToken === method.token && styles.paymentOptionSelected]}
        >
          <View style={[styles.radio, paymentToken === method.token && styles.radioSelected]} />
          <View style={styles.paymentText}>
            <Text style={styles.paymentLabel}>{method.label}</Text>
            <Text style={styles.paymentHint}>{method.hint}</Text>
          </View>
        </Pressable>
      ))}

      <View style={styles.summary}>
        <Text style={styles.summaryLabel}>Total do pedido</Text>
        <Text style={styles.summaryValue}>{formatMoney(total, currency)}</Text>
      </View>

      {error && <Text style={styles.submitError}>{error}</Text>}

      <PrimaryButton label="Confirmar pedido" onPress={handleSubmit} loading={submitting} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg, gap: spacing.sm, paddingBottom: spacing.xxxl },
  sectionTitle: { ...typography.title, color: colors.textPrimary, marginTop: spacing.lg, marginBottom: spacing.xs },
  row: { flexDirection: 'row', gap: spacing.sm },
  flex1: { flex: 1 },
  flex2: { flex: 2 },
  errorBox: { backgroundColor: colors.dangerSoft, borderRadius: radius.sm, padding: spacing.sm, marginTop: spacing.xs },
  errorText: { ...typography.caption, color: colors.danger },
  paymentOption: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    padding: spacing.md,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.surface,
    marginBottom: spacing.sm,
  },
  paymentOptionSelected: { borderColor: colors.accent, backgroundColor: colors.accentSoft },
  radio: { width: 18, height: 18, borderRadius: 9, borderWidth: 2, borderColor: colors.border },
  radioSelected: { borderColor: colors.accent, backgroundColor: colors.accent },
  paymentText: { flex: 1 },
  paymentLabel: { ...typography.subtitle, color: colors.textPrimary },
  paymentHint: { ...typography.caption, color: colors.textSecondary },
  summary: { flexDirection: 'row', justifyContent: 'space-between', marginVertical: spacing.lg },
  summaryLabel: { ...typography.subtitle, color: colors.textSecondary },
  summaryValue: { ...typography.title, color: colors.textPrimary },
  submitError: { ...typography.caption, color: colors.danger, marginBottom: spacing.sm, textAlign: 'center' },
});
