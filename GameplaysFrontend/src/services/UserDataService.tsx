import { LoaderFunctionArgs } from 'react-router-dom';
import { User } from '../types/UserType';

const apiUrl = 'https://localhost:5001'

async function registerUser(data: User) {
    try {
        const response = await fetch(`${apiUrl}/api/users/register`, {
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
        const response = await fetch(`${apiUrl}/api/users/login`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data),
            //credentials: 'include'
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
        const response = await fetch(`${apiUrl}/api/users/${params.id}`, {
            method: 'GET',
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

async function updateUser(data: User) {
    try {
        const response = await fetch(`${apiUrl}/api/users/profile`, {
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
