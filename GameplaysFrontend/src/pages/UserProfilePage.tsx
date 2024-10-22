import { useLoaderData } from "react-router-dom";
import { UserType } from "../types/DataType";

function UserProfilePage() {
    const user = useLoaderData() as UserType;

    return (
        <div>User: {user.username}</div>
    );
};

export default UserProfilePage;
