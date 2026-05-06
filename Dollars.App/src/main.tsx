import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import App from './App.tsx'
import Logs from './Logs.tsx';
import NotFound from './NotFound.tsx'

const router = createBrowserRouter([
  { path: '/', element: <App /> },
  
  { path: '/logs', element: <Logs />, loader: () => ({ page: 1 }) },
  { path: '/logs/:page', element: <Logs /> },
  { path: '*', element: <NotFound /> }
]);

createRoot(document.getElementById('root')!).render(
  // <StrictMode>
    <RouterProvider router={router}/>
  // </StrictMode>
)
