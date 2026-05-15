import { Route, Routes } from "react-router-dom"
import { ProtectedRoute } from "@/components/protected-route"
import LoginPage from "@/pages/login"
import Home from "@/pages/home"
import Dashboard from "@/pages/dashboard"
import TransactionsPage from "@/pages/transactions"

const App = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/" element={<ProtectedRoute><Home /></ProtectedRoute>}>
        <Route index element={<Dashboard />} />
        <Route path="transactions" element={<TransactionsPage />} />
      </Route>
    </Routes>
  )
}

export default App
