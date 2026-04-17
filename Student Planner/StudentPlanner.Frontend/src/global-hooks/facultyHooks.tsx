import { useQuery } from "@tanstack/react-query";
import { getAllFaculties, getFacultyById } from "../api/facultyApi";
import type { facultyResponse } from "../types/facultyTypes";


export function useFaculty({facultyId}: {facultyId: string}){
    const {data, isPending} = useQuery<facultyResponse>({
        queryKey: ["faculty", facultyId],
        queryFn: () => getFacultyById(facultyId)
    })

    return {
        faculty: data,
        isPending
    }
}

export function useFaculties(){
    const {data, isPending} = useQuery<facultyResponse[]>({
        queryKey: ["faculty", "all"],
        queryFn: getAllFaculties
    })

    return {
        faculties: data?? [],
        isPending
    }
}