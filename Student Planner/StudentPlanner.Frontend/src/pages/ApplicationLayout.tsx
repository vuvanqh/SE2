import { Outlet } from "react-router-dom"
import Footer from "../components/layout/Footer"
import { ToastContainer } from 'react-toastify';

export default function ApplicationLayout(){
    return <>
        <Outlet/>
        <ToastContainer />
        <Footer/>
    </>
}