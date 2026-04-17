import { apiClient } from "./apiClient";

const facultyUrl = "/faculty";

export const getAllFaculties = async () => (await apiClient.get(facultyUrl)).data;
export const getFacultyById = async (facultyId: string) => (await apiClient.get(facultyUrl + `/${facultyId}`)).data;
