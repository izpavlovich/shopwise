import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5173,
    strictPort: true,
    watch: {
      usePolling: true,
      interval: 300,
    },
    proxy: {
      '/api': {
        target: 'http://api:8080',
        changeOrigin: true,
      },
      '/images': {
        target: 'http://api:8080',
        changeOrigin: true,
      },
      '/recommendations': {
        target: 'http://recommendations:8000',
        changeOrigin: true,
      },
    },
  },
})
