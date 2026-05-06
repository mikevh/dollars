import { useEffect, useState } from "react";
import { useParams, useLoaderData } from "react-router-dom";

type LogRow = {
    Id: number,
}

const Logs = () => {
    const { page: pageParam } = useParams();
    const loaderData = useLoaderData() as { page: number } | null;
    const page = pageParam ? Number(pageParam) : (loaderData?.page ?? 1);
    const [rows, setRows] = useState<LogRow[]>([]);

    useEffect(() => {
        fetch(`/api/logs/${page}`)
        .then(r => r.json())        
        .then(d => { console.log(d); setRows(d); })
        .catch(e => console.error(`error fetching page ${page} of log rows:`, e))
    }, [page]);

    return ( 
        <h2>Page {page}</h2>
    );

}

export default Logs;