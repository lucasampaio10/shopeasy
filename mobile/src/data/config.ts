/**
 * Base URL of the ShopEasy API Gateway. `EXPO_PUBLIC_` variables are inlined into the bundle by
 * Expo at build time, so this can be overridden per environment (device on the LAN, staging,
 * production) without touching code — see mobile/.env.example.
 */
export const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:8080';
