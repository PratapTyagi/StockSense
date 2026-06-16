import type { Config } from 'tailwindcss'

export default {
  darkMode: 'class',
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      colors: {
        bg: '#f5f5f5',
        card: '#ffffff',
        pos: '#22c55e',
        neg: '#ef4444',
        muted: '#64748b',
        border: 'rgba(15, 23, 42, 0.08)',
      },
      boxShadow: {
        card: '0 10px 28px rgba(15, 23, 42, 0.08)',
      },
      borderRadius: {
        card: '16px',
      },
    },
  },
  plugins: [require('@tailwindcss/forms')],
} satisfies Config

