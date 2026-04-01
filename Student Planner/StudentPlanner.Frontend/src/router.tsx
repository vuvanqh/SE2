import { createBrowserRouter } from "react-router-dom";
import IntroPage from "./pages/public/IntroPage";
import ProtectedRoute from "./components/routing/ProtectedRoute";
import LoginPage from "./pages/public/LoginPage";
import RegisterPage from "./pages/public/RegisterPage";
import ApplicationLayout from "./pages/ApplicationLayout";
import StudentPage from "./pages/protected/StudentPage";
import RoleRoute from "./components/routing/RoleRoute";

export const router = createBrowserRouter([{
    path: "/",
    element: <ApplicationLayout/>,
    children:[
        {
            path:"",
            element: <IntroPage/>,
            children: [
            {
                path: "login",
                element: <LoginPage/>
            },
            {
                path: "register",
                element: <RegisterPage/>
            }
        ]},
        {
            path: "",
            element: <ProtectedRoute/>,
            children: [
                {
                    path: "student",
                    element: <RoleRoute allowed={["Student"]}>
                        <StudentPage/>
                    </RoleRoute>
                },
                {
                    path: "manager",
                    element: <RoleRoute allowed={["Manager"]}>
                        <StudentPage/>
                    </RoleRoute>
                },
                {
                    path: "admin",
                    element: <RoleRoute allowed={["Admin"]}>
                        <StudentPage/>
                    </RoleRoute>
                }
            ]
            
        }
    ]
}])