import {
  createBrowserRouter,
  createRoutesFromElements,
  RouterProvider,
  Route
} from 'react-router-dom';
import AboutPage from './pages/AboutPage';
import GamesPage from './pages/GamesPage';
import HelpPage from './pages/HelpPage';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import MainLayout from './layouts/MainLayout';
import NotFoundPage from './pages/NotFoundPage';
import PrivacyPage from './pages/PrivacyPage';
import PrivateRoute from './components/PrivateRoute';
import ProfilePage from './pages/ProfilePage';
import RegisterPage from './pages/RegisterPage';
import TosPage from './pages/TosPage';

function App() {
  const router = createBrowserRouter(
    createRoutesFromElements(
      <Route path='/' element={<MainLayout />}>
        <Route index element={<HomePage />} />

        {/* Auth Routes */}
        <Route path='/login' element={<LoginPage />} />
        <Route path='/register' element={<RegisterPage />} />
        
        {/* Private Routes */}
        <Route path='' element={<PrivateRoute />}>
          <Route path='/profile' element={<ProfilePage />} />
          <Route path='/games' element={<GamesPage />} />
        </Route>

        {/* Info Routes */}
        <Route path='/about' element={<AboutPage />} />
        <Route path='/help' element={<HelpPage />} />
        <Route path='/privacy' element={<PrivacyPage />} />
        <Route path='/tos' element={<TosPage />} />

        <Route path='*' element={<NotFoundPage />} />
      </Route>
    )
  );
  return <RouterProvider router={router} />;
}

export default App;
