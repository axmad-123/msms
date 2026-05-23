import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { Role } from '../types';

export function ProtectedRoute({ children, role }: { children: React.ReactNode; role: Role }) {
  const { isAuthenticated, hasRole } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (!hasRole(role)) return <Navigate to="/login" replace />;
  return children;
}
