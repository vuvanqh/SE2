import { createBrowserRouter } from "react-router-dom";
import IntroPage from "./pages/public/IntroPage";
import ProtectedRoute from "./components/routing/ProtectedRoute";
import LoginPage from "./pages/public/LoginPage";
import RegisterPage from "./pages/public/RegisterPage";
import ApplicationLayout from "./pages/ApplicationLayout";
import StudentPage from "./pages/protected/Student/StudentPage";
import RoleRoute from "./components/routing/RoleRoute";
import AdminPage from "./pages/protected/AdminPage";
import ManagerPage from "./pages/protected/Manager/ManagerPage";
import StudentCalendarPage from "./pages/protected/Student/StudentCalendarPage";
import ManagerCalendarPage from "./pages/protected/Manager/ManagerCalendarPage";

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
                    </RoleRoute>,
                    children: [
                        {
                            index: true,
                            element: <StudentCalendarPage/>
                        },
                        {
                            path: "events",
                            element: <StudentCalendarPage/>
                        },
                        {
                            path: "requests",
                            element: <StudentCalendarPage/>
                        }
                    ]
                },
                {
                    path: "manager",
                    element: <RoleRoute allowed={["Manager"]}>
                        <ManagerPage/>
                    </RoleRoute>,
                    children: [
                        {
                            index: true,
                            element: <ManagerCalendarPage/>
                        },
                        {
                            path: "events",
                            element: <ManagerCalendarPage/> //TO-DO idiot
                        },
                        {
                            path: "requests",
                            element: <ManagerCalendarPage/>
                        }
                    ]
                },
                {
                    path: "admin",
                    element: <RoleRoute allowed={["Admin"]}>
                        <AdminPage/>
                    </RoleRoute>
                }
            ]
            
        }
    ]
}])