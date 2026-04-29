export type createManagerRequest = {
    firstName: string;
    lastName: string;
    email: string;
    facultyId?: string;
}

export type userResponse = {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    userRole: "Admin" | "Manager" | "User";
    faculty: string;
    facultyId?: string;
    facultyCode?: string;
}