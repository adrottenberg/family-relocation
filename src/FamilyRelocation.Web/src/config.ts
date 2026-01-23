/**
 * Runtime configuration for the application
 * Values are set from environment variables at build time
 */
export const config = {
  /**
   * Base URL for API requests
   * In development: /api (proxied by Vite)
   * In production: https://dev.unionvaad.com/api
   */
  apiBaseUrl: import.meta.env.VITE_API_URL || '/api',

  /**
   * Current environment
   */
  environment: import.meta.env.MODE as 'development' | 'production' | 'test',

  /**
   * Whether the app is running in production mode
   */
  isProduction: import.meta.env.PROD,

  /**
   * Whether the app is running in development mode
   */
  isDevelopment: import.meta.env.DEV,
} as const;

// Type for config to use in other files
export type Config = typeof config;
