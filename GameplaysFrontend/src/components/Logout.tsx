import { useNavigate } from "react-router-dom";
import { logoutUser } from "../services/UserDataService";
import { useEffect } from "react";

function Logout() {
    const navigate = useNavigate();
    
    // sync navigate to logout
    useEffect(() => {
        const performLogout = async () => {    
            try {
                await logoutUser();
                navigate('/');
            } catch (error) {
                throw error;
            }
        } 
        performLogout();
    }, [navigate]);

    // maybe include spinner here
    return (
        <></>
    )
}

export default Logout;
