import { createContext, useContext, useState } from "react"

// thing being stored in the context
interface AuthContextValue {
  token: string | null
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  // lazy init call to localstorage insead of render
  const [token, setToken] = useState<string | null>(() => localStorage.getItem("token"))

  async function login(email: string, password: string) {
    const res = await fetch("/api/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    })

    if (!res.ok) {
      throw new Error("Invalid email or password")
    }

    const data = await res.json()
    localStorage.setItem("token", data.token)
    setToken(data.token)
  }

  function logout() {
    localStorage.removeItem("token")
    setToken(null)
  }

  return (
    <AuthContext.Provider value={{ token, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider")
  }
  return ctx;
}
