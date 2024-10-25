import { AuthContext } from '../contexts/AuthContext';
import { useContext } from 'react';
import Hero from '../components/Hero';
import Start from './Start';

function HomePage() {
    const { isLoggedInContext } = useContext(AuthContext);

    if (isLoggedInContext) {
        return (
            <Start />
        );
    } else {
        return (
            <Hero />
        );
    }
}

export default HomePage;
