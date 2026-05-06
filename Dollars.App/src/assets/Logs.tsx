import { useState, useEffect } from 'react';

type Log = {
    Id: number,
    Message: string,
    Level: string,
    TimeStamp: Date,
    Exception: string
}

const Logs = () => {
    const [page, setPage] = useState(1);
    const [rows, setRows] = useState<Log[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    const getPage = (page: number) => {
        fetch(`/logs/${page}`)
        .then(r => {
            if(!r.ok) throw new Error(`${r.status} ${r.statusText}`)
            return r.json() as Promise<Log[]>
        })
        .then(setRows)
        .catch((e: Error) => setError(e.message))
        .finally(() => setLoading(false))
    };

    useEffect(() => {
        getPage(page)
    }, []);

    if(loading) return <p>Loading...</p>
    if(error) return <p>Error: {error}</p>

return (
    <table>
        <thead>
            <tr>
                <th>Id</th>
                <th>Message</th>
                <th>Level</th>
                <th>Time</th>
                <th>Ex</th>
            </tr>
        </thead>
        <tbody>
            {rows.map(r => (
                <tr key={r.Id}>
                    <td>{r.Id}</td>
                    <td>{r.Message}</td>
                    <td>{r.Level}</td>
                    <td>{r.TimeStamp.toLocaleTimeString()}</td>
                    <td>{r.Exception}</td>
                </tr>
            ))}
        </tbody>
    </table>
);

}

export default Logs;