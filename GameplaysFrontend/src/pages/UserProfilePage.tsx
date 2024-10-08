import { useLoaderData } from "react-router-dom";
import { User } from "../types/UserType";

function UserProfilePage() {
    const user = useLoaderData() as User;

    return (
        <div>User: {user.username}</div>
    );
};

export default UserProfilePage;
