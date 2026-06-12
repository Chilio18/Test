export default defineNuxtRouteMiddleware((to) => {
  if (process.client) {
    const token = localStorage.getItem('auth_token')
    if (!token && to.path !== '/login') {
      return navigateTo('/login')
    }
  }
})
