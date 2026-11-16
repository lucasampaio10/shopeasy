/**
 * Design tokens for the ShopEasy storefront. A deep teal accent and warm neutrals, chosen on
 * purpose so the app doesn't default to the generic indigo/purple gradient every scaffold reaches
 * for — this is a tech/gadget store, not a SaaS dashboard.
 */
export const colors = {
  background: '#faf9f6',
  surface: '#ffffff',
  surfaceMuted: '#f1efe8',
  border: '#e4e1d6',

  textPrimary: '#1a1a17',
  textSecondary: '#625f54',
  textMuted: '#9a978a',
  textOnAccent: '#ffffff',

  accent: '#0f8b6c',
  accentPressed: '#0c7059',
  accentSoft: '#e1f3ec',

  danger: '#b3261e',
  dangerSoft: '#fbe9e6',
  warning: '#a66a00',
  warningSoft: '#fcefd9',
  success: '#1b7a3d',
  successSoft: '#e3f3e6',
} as const;

export const spacing = {
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 24,
  xxl: 32,
  xxxl: 48,
} as const;

export const radius = {
  sm: 8,
  md: 12,
  lg: 16,
  pill: 999,
} as const;

export const typography = {
  display: { fontSize: 26, fontWeight: '800' as const, letterSpacing: -0.3 },
  title: { fontSize: 19, fontWeight: '700' as const },
  subtitle: { fontSize: 15, fontWeight: '600' as const },
  body: { fontSize: 15, fontWeight: '400' as const },
  caption: { fontSize: 13, fontWeight: '400' as const },
  label: { fontSize: 12, fontWeight: '700' as const, letterSpacing: 0.4 },
};
