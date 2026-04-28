import { type ReactElement, type ReactNode } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { render, type RenderOptions } from "@testing-library/react";
import ModalContextProvider from "../src/store/ModalContext";

export function makeQueryClient() {
    return new QueryClient({
        defaultOptions: {
            queries: { retry: false, staleTime: 0, gcTime: 0 },
            mutations: { retry: false },
        },
    });
}

type Options = {
    route?: string;
    routes?: { path: string; element: ReactNode }[];
    withModalContext?: boolean;
    queryClient?: QueryClient;
} & Omit<RenderOptions, "wrapper">;

export function renderWithProviders(ui: ReactElement, options: Options = {}) {
    const {
        route = "/",
        routes = [],
        withModalContext = false,
        queryClient = makeQueryClient(),
        ...rest
    } = options;

    function Wrapper({ children }: { children: ReactNode }) {
        const tree = (
            <QueryClientProvider client={queryClient}>
                <MemoryRouter initialEntries={[route]}>
                    <Routes>
                        <Route path="/" element={children} />
                        {routes.map((r) => (
                            <Route key={r.path} path={r.path} element={r.element} />
                        ))}
                    </Routes>
                </MemoryRouter>
            </QueryClientProvider>
        );
        return withModalContext ? <ModalContextProvider>{tree}</ModalContextProvider> : tree;
    }

    return { ...render(ui, { wrapper: Wrapper, ...rest }), queryClient };
}

export function withQueryClient(children: ReactNode, queryClient = makeQueryClient()) {
    return (
        <QueryClientProvider client={queryClient}>
            <MemoryRouter>{children}</MemoryRouter>
        </QueryClientProvider>
    );
}
