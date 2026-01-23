import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // Load env file based on mode
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src'),
      },
    },
    // Define environment variables to be exposed to the client
    define: {
      __APP_VERSION__: JSON.stringify(process.env.npm_package_version),
    },
    build: {
      // Generate source maps for production debugging
      sourcemap: mode !== 'production',
      // Output directory
      outDir: 'dist',
    },
    server: {
      port: 3000,
      // Only use proxy in development when VITE_API_URL is not set or is /api
      proxy: !env.VITE_API_URL || env.VITE_API_URL === '/api'
        ? {
            '/api': {
              target: 'https://localhost:7267',
              changeOrigin: true,
              secure: false, // Accept self-signed certs in development
              rewrite: (path) => path,
              configure: (proxy, _options) => {
                proxy.on('error', (err, _req, _res) => {
                  console.log('proxy error', err);
                });
                proxy.on('proxyReq', (proxyReq, req, _res) => {
                  console.log('Sending Request:', req.method, req.url);
                });
                proxy.on('proxyRes', (proxyRes, req, _res) => {
                  console.log('Received Response:', proxyRes.statusCode, req.url);
                });
              },
            },
          }
        : undefined,
    },
  };
});
