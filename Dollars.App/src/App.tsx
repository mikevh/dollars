import { useEffect, useState } from 'react'
import './App.css'
import ButtonPage from './ButtonPage'

type Transaction = {
  id: number
  accountId: number
  sourceId: string
  payee: string
  date: string
  amount: number
  description: string
  memo: string
}

const App = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetch('/api/transactions')
      .then(r => {
        if (!r.ok) throw new Error(`${r.status} ${r.statusText}`)
        return r.json() as Promise<Transaction[]>
      })
      .then(setTransactions)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <p>Loading...</p>
  if (error) return <p>Error: {error}</p>

  return (
    <>
    <ButtonPage />
    <table>
      <thead>
        <tr>
          <th>Date</th>
          <th>Payee</th>
          <th>Amount</th>
          <th>Description</th>
        </tr>
      </thead>
      <tbody>
        {transactions.map(t => (
          <tr key={t.id}>
            <td>{new Date(t.date).toLocaleDateString()}</td>
            <td>{t.payee}</td>
            <td style={{ color: t.amount < 0 ? 'red' : 'green' }}>
              {t.amount.toFixed(2)}
            </td>
            <td>{t.description}</td>
          </tr>
        ))}
      </tbody>
    </table>
    </>
  )
}

export default App