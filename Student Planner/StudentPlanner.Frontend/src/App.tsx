import './styles/App.css'
import { RouterProvider } from 'react-router-dom'
import { router } from './router'
import { queryClient } from './api/queryClient'
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'


function App() {
  return <QueryClientProvider client={queryClient}>
    <RouterProvider router={router}/>
    <ReactQueryDevtools initialIsOpen={false} />
  </QueryClientProvider>
}

export default App