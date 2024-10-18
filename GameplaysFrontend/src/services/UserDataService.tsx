import { LoaderFunctionArgs } from 'react-router-dom';
import { User } from '../types/UserType';

const apiUrl = 'http://localhost:5000/api'

async function registerUser(data: User) {
    try {
        const response = await fetch(`${apiUrl}/users/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return response;

    } catch (error) {
        throw error;
    }
}

async function authUser(data: User) {
    try {
        const response = await fetch(`${apiUrl}/users/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data),
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const user: User = await response.json();
        return user;

    } catch (error) {
        throw error;
    }
}

// alter to fetch from /api/users/profile ???
async function fetchUser({params}: LoaderFunctionArgs) {
    try {
        const response = await fetch(`${apiUrl}/users/${params.id}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const user: User = await response.json();
        return user;

    } catch (error) {
        throw error;
    }
}

async function updateUser(data: User) {
    try {
        const response = await fetch(`${apiUrl}/users/profile`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return response;

    } catch (error) {
        throw error;
    }
}

export { fetchUser, authUser, registerUser, updateUser };
