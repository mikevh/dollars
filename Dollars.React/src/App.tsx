import { Route, Routes } from "react-router-dom"
import { ProtectedRoute } from "@/components/protected-route"
import LoginPage from "@/pages/login"
import Home from "@/pages/home"

const App = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/*" element={<ProtectedRoute><Home /></ProtectedRoute>}/>
    </Routes>
  )
}

export default App
