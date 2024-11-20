import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react-swc';

let serverTarget = "";

if (process.env.NODE_ENV === 'development') {
  serverTarget = "https://localhost:5001";
} else {
  serverTarget = "https://localhost:8001";
}

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0',
    port: 3000,
    proxy: {
      '/api': {
        target: serverTarget,
        changeOrigin: true
      }
    }
  }
});
