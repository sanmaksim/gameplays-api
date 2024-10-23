import {
  createBrowserRouter,
  createRoutesFromElements,
  RouterProvider,
  Route
} from 'react-router-dom';
import { fetchUser } from './services/UserDataService';
import AboutPage from './pages/AboutPage';
import HelpPage from './pages/HelpPage';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import MainLayout from './layouts/MainLayout';
import MyGamesPage from './pages/MyGamesPage';
import NotFoundPage from './pages/NotFoundPage';
import PrivacyPage from './pages/PrivacyPage';
import RegisterPage from './pages/RegisterPage';
import TosPage from './pages/TosPage';
import UserProfilePage from './pages/UserProfilePage';
import Logout from './components/Logout';

function App() {
  const router = createBrowserRouter(
    createRoutesFromElements(
      <Route path='/' element={<MainLayout />}>
        <Route index element={<HomePage />} />
        <Route path='/about' element={<AboutPage />} />
        <Route path='/help' element={<HelpPage />} />
        <Route path='/privacy' element={<PrivacyPage />} />
        <Route path='/tos' element={<TosPage />} />
        <Route path='/user/login' element={<LoginPage />} />
        <Route path='/user/games' element={<MyGamesPage />} />
        <Route path='/user/:id' element={<UserProfilePage />} loader={ fetchUser } />
        <Route path='/user/register' element={<RegisterPage />} />
        <Route path='/user/logout' element={<Logout />} />
        <Route path='*' element={<NotFoundPage />} />
      </Route>
    )
  );
  return <RouterProvider router={router} />;
}

export default App;
