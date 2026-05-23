import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { DashboardLayout } from './components/layout/DashboardLayout';
import { ProtectedRoute } from './routes/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { AdminDashboard } from './pages/admin/AdminDashboard';
import { StudentsPage } from './pages/admin/StudentsPage';
import { ParentsPage } from './pages/admin/ParentsPage';
import { TeachersPage } from './pages/admin/TeachersPage';
import { SchoolPage } from './pages/admin/SchoolPage';
import { AttendancePage } from './pages/admin/AttendancePage';
import { ExamsPage } from './pages/admin/ExamsPage';
import { FinancePage } from './pages/admin/FinancePage';
import { GraduatesPage } from './pages/admin/GraduatesPage';
import { TeacherDashboard } from './pages/teacher/TeacherDashboard';
import { TeacherAssignmentsPage } from './pages/teacher/TeacherAssignmentsPage';
import { TeacherAttendancePage } from './pages/teacher/TeacherAttendancePage';
import { TeacherExamsPage } from './pages/teacher/TeacherExamsPage';
import { ParentDashboard } from './pages/parent/ParentDashboard';
import { ChildrenPage } from './pages/parent/ChildrenPage';
import { ParentAttendancePage } from './pages/parent/ParentAttendancePage';
import { ParentPaymentsPage } from './pages/parent/ParentPaymentsPage';
import { ParentResultsPage } from './pages/parent/ParentResultsPage';
import { StudentDashboard } from './pages/student/StudentDashboard';
import { StudentResultsPage } from './pages/student/StudentResultsPage';

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          <Route
            path="/admin"
            element={
              <ProtectedRoute role="Admin">
                <DashboardLayout role="Admin" />
              </ProtectedRoute>
            }
          >
            <Route index element={<AdminDashboard />} />
            <Route path="students" element={<StudentsPage />} />
            <Route path="parents" element={<ParentsPage />} />
            <Route path="teachers" element={<TeachersPage />} />
            <Route path="school" element={<SchoolPage />} />
            <Route path="attendance" element={<AttendancePage />} />
            <Route path="exams" element={<ExamsPage />} />
            <Route path="finance" element={<FinancePage />} />
            <Route path="graduates" element={<GraduatesPage />} />
          </Route>

          <Route
            path="/teacher"
            element={
              <ProtectedRoute role="Teacher">
                <DashboardLayout role="Teacher" />
              </ProtectedRoute>
            }
          >
            <Route index element={<TeacherDashboard />} />
            <Route path="assignments" element={<TeacherAssignmentsPage />} />
            <Route path="attendance" element={<TeacherAttendancePage />} />
            <Route path="exams" element={<TeacherExamsPage />} />
          </Route>

          <Route
            path="/parent"
            element={
              <ProtectedRoute role="Parent">
                <DashboardLayout role="Parent" />
              </ProtectedRoute>
            }
          >
            <Route index element={<ParentDashboard />} />
            <Route path="children" element={<ChildrenPage />} />
            <Route path="attendance" element={<ParentAttendancePage />} />
            <Route path="payments" element={<ParentPaymentsPage />} />
            <Route path="results" element={<ParentResultsPage />} />
          </Route>

          <Route
            path="/student"
            element={
              <ProtectedRoute role="Student">
                <DashboardLayout role="Student" />
              </ProtectedRoute>
            }
          >
            <Route index element={<StudentDashboard />} />
            <Route path="results" element={<StudentResultsPage />} />
          </Route>

          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
