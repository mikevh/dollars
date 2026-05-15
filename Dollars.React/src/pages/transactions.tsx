import { useEffect, useState } from "react"
import type { ColumnDef, SortingState } from "@tanstack/react-table"
import {
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
} from "@tanstack/react-table"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { apiFetch } from "@/lib/api"

type Transaction = {
  id: number
  accountId: number
  payee: string
  date: string
  amount: number
  description: string
  memo: string
}

const columns: ColumnDef<Transaction>[] = [
  {
    accessorKey: "date", header: "Date",
    cell: ({ getValue }) => new Date(getValue<string>()).toLocaleDateString()
  },
  { accessorKey: "payee", header: "Payee" },
  { accessorKey: "description", header: "Description" },
  {
    accessorKey: "amount", header: "Amount",
    cell: ({ getValue }) => {
      const amount = getValue<number>()
      const formatted = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      }).format(Math.abs(amount))
      return (
        <span className={amount < 0 ? "text-red-500" : "text-green-600"}>
          {amount < 0 ? `-${formatted}` : formatted}
        </span>
      )
    }
  }
]

const TransactionsPage = () => {
  const [data, setData] = useState<Transaction[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [sorting, setSorting] = useState<SortingState>([])

  useEffect(() => {
    apiFetch("/api/transactions")
      .then(r => {
        if (!r.ok) {
          throw new Error(`${r.status} ${r.statusText}`)
        }
        return r.json()
      })
      .then(setData)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  const table = useReactTable({
    data,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  })

  return (
    <div className="flex flex-1 flex-col gap-4 p-6">
      <h1 className="text-2xl font-semibold">Transactions</h1>
      {loading && <p className="text-muted-foreground">Loading…</p>}
      {error && <p className="text-red-500">Error: {error}</p>}
      {!loading && !error && (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              {table.getHeaderGroups().map((hg) => (
                <TableRow key={hg.id}>
                  {hg.headers.map((header) => (
                    <TableHead key={header.id}>
                      {flexRender(header.column.columnDef.header, header.getContext())}
                    </TableHead>
                  ))}
                </TableRow>
              ))}
            </TableHeader>
            <TableBody>
              {table.getRowModel().rows.length ? (
                table.getRowModel().rows.map((row) => (
                  <TableRow key={row.id}>
                    {row.getVisibleCells().map((cell) => (
                      <TableCell key={cell.id}>
                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={columns.length} className="h-24 text-center text-muted-foreground">
                    No transactions
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  )
}

export default TransactionsPage;