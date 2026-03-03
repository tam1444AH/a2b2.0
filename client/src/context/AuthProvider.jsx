import React, { createContext, useEffect, useState } from "react";
import { jwtDecode } from "jwt-decode";

export const AuthContext = createContext();

const AuthProvider = ({ children }) => {

    const [isLoggedIn, setIsLoggedIn] = useState(() => {
        const token = localStorage.getItem("authToken");
        return !!token;
    });

    const logOut = () => {
        localStorage.removeItem('authToken');
        setIsLoggedIn(false);
    };

    const isTokenExpired = (token) => {
        try {
            const { exp } = jwtDecode(token);
            const now = Date.now() / 1000;
            return exp < now;
        } catch (error) {
            console.error("Failed to decode token:", error);
            return true;
        }
    };

    useEffect(() => {
        const token = localStorage.getItem("authToken");
        if (token && isTokenExpired(token)) {
            logOut();
        }
    }, []);

    return (
        <AuthContext.Provider value={{ isLoggedIn, setIsLoggedIn, logOut }}>
            {children}
        </AuthContext.Provider>
    );
};

export default AuthProvider;