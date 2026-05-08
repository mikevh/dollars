import { useState } from "react"
import { Navigate, useNavigate } from "react-router-dom"
import { LoginForm } from "@/components/login-form"
import { useAuth } from "@/lib/auth"

const LoginPage = () => {
  const { token, login } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string>()

  if (token) {
    return <Navigate to="/" replace />
  }

  async function handleLogin(email: string, password: string) {
    setError(undefined)
    try {
      await login(email, password)
      navigate("/", { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed")
    }
  }

  return (
    <div className="flex min-h-svh items-center justify-center p-6">
      <LoginForm className="w-full max-w-sm" 
        onLogin={handleLogin} 
        error={error} />
    </div>
  )
}

export default LoginPage