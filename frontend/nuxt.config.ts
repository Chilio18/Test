export default defineNuxtConfig({
  devtools: { enabled: true },

  modules: [
    '@nuxt/ui',
    '@nuxtjs/color-mode',
    '@pinia/nuxt',
    '@vee-validate/nuxt',
    '@nuxtjs/tailwindcss',
  ],

  colorMode: {
    classSuffix: '',
    preference: 'dark',
    fallback: 'dark',
  },

  ui: {
    global: true,
    icons: ['heroicons', 'lucide'],
  },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE ?? 'http://localhost:5000',
      signalrBase: process.env.NUXT_PUBLIC_SIGNALR_BASE ?? 'http://localhost:5000',
    },
  },

  typescript: { strict: true, typeCheck: false },

  app: {
    head: {
      title: 'Revenue Intelligence — UnameIT',
      meta: [{ name: 'viewport', content: 'width=device-width, initial-scale=1' }],
    },
    pageTransition: { name: 'page', mode: 'out-in' },
  },

  srcDir: 'app/',
})
