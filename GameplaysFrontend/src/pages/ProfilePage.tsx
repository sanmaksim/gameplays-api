import { Container } from "react-bootstrap";
import { useLoaderData } from "react-router-dom";
import { UserType } from "../types/DataType";

function ProfilePage() {
    const user = useLoaderData() as UserType;

    return (
        <Container>
            User: {user.username}
        </Container>
    );
};

export default ProfilePage;
