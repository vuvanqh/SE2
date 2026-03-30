import { type ReactNode, useEffect, useState, useRef } from 'react';

type NavbarProps = {
    children: ReactNode
}

export default function Navbar({children}: NavbarProps) {
    const [hidden, setHidden] = useState(false);
    const lastScrollY = useRef(0);

    useEffect(() => {
        function onScroll() {
        const currentY = window.scrollY;

        if (currentY > lastScrollY.current && currentY > 80) {
            setHidden(true);
        } else {
            setHidden(false);
        }

        lastScrollY.current = currentY;
        }

        window.addEventListener("scroll", onScroll, { passive: true });
        return () => window.removeEventListener("scroll", onScroll);
    }, []);
    return <nav className={`navbar ${hidden ? "navbar-hidden" : ""}`}>
        {children}
    </nav>
}

