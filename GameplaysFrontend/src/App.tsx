import {
  createBrowserRouter,
  createRoutesFromElements,
  RouterProvider,
  Route
} from 'react-router-dom';
import { fetchUser } from './services/UserDataService';
// import Logout from './components/Logout';
import MainLayout from './layouts/MainLayout';
import AboutPage from './pages/AboutPage';
import HelpPage from './pages/HelpPage';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import MyGamesPage from './pages/MyGamesPage';
import NotFoundPage from './pages/NotFoundPage';
import PrivacyPage from './pages/PrivacyPage';
import ProfilePage from './pages/ProfilePage';
import RegisterPage from './pages/RegisterPage';
import SettingsPage from './pages/SettingsPage';
import TosPage from './pages/TosPage';

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
        <Route path='/user/profile' element={<ProfilePage />} loader={ fetchUser } />
        <Route path='/user/register' element={<RegisterPage />} />
        <Route path='/user/settings' element={<SettingsPage />} />
        <Route path='*' element={<NotFoundPage />} />
      </Route>
    )
  );
  return <RouterProvider router={router} />;
}

export default App;
