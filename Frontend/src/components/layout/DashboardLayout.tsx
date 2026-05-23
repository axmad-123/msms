import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import {
  BookOpen,
  CalendarCheck,
  CreditCard,
  GraduationCap,
  LayoutDashboard,
  LogOut,
  School,
  Users,
  ClipboardList,
  UserCircle,
  Bell,
  Search,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import type { Role } from '../../types';

type NavItem = { to: string; label: string; icon: LucideIcon };

const NAV: Record<Role, NavItem[]> = {
  Admin: [
    { to: '/admin', label: 'Dashboard', icon: LayoutDashboard },
    { to: '/admin/students', label: 'Students', icon: Users },
    { to: '/admin/parents', label: 'Parents', icon: UserCircle },
    { to: '/admin/teachers', label: 'Teachers', icon: GraduationCap },
    { to: '/admin/school', label: 'Classes & Subjects', icon: School },
    { to: '/admin/attendance', label: 'Attendance', icon: CalendarCheck },
    { to: '/admin/exams', label: 'Exams', icon: BookOpen },
    { to: '/admin/finance', label: 'Finance', icon: CreditCard },
    { to: '/admin/graduates', label: 'Graduates', icon: GraduationCap },
  ],
  Teacher: [
    { to: '/teacher', label: 'Dashboard', icon: LayoutDashboard },
    { to: '/teacher/assignments', label: 'My Classes', icon: School },
    { to: '/teacher/attendance', label: 'Attendance', icon: CalendarCheck },
    { to: '/teacher/exams', label: 'Enter Marks', icon: ClipboardList },
  ],
  Parent: [
    { to: '/parent', label: 'Dashboard', icon: LayoutDashboard },
    { to: '/parent/children', label: 'My Children', icon: Users },
    { to: '/parent/attendance', label: 'Attendance', icon: CalendarCheck },
    { to: '/parent/payments', label: 'Payments', icon: CreditCard },
    { to: '/parent/results', label: 'Results', icon: BookOpen },
  ],
  Student: [
    { to: '/student', label: 'Dashboard', icon: LayoutDashboard },
    { to: '/student/results', label: 'My Results', icon: BookOpen },
  ],
};

const roleColors: Record<Role, string> = {
  Admin: 'from-violet-500 to-indigo-600',
  Teacher: 'from-sky-500 to-blue-600',
  Parent: 'from-emerald-500 to-teal-600',
  Student: 'from-amber-500 to-orange-600',
};

export function DashboardLayout({ role }: { role: Role }) {
  const { logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const items = NAV[role];

  const currentPage = items.find((item) => {
    if (item.to === `/${role.toLowerCase()}`) {
      return location.pathname === item.to;
    }
    return location.pathname.startsWith(item.to);
  });

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex min-h-screen gradient-mesh">
      {/* Sidebar */}
      <aside className="sidebar-gradient fixed inset-y-0 left-0 z-30 flex w-[260px] flex-col border-r border-white/5 shadow-2xl">
        <div className="border-b border-white/10 px-5 py-6">
          <div className="flex items-center gap-3">
            <span className={`flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br ${roleColors[role]} shadow-lg shadow-indigo-500/30`}>
              <School className="h-5 w-5 text-white" strokeWidth={2} />
            </span>
            <div>
              <p className="text-lg font-bold tracking-tight text-white">MSMS</p>
              <p className="text-xs font-medium text-indigo-200/80">School Management</p>
            </div>
          </div>
        </div>

        <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-4">
          <p className="mb-2 px-3 text-[10px] font-bold uppercase tracking-widest text-slate-500">Menu</p>
          {items.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to.split('/').length <= 2}
              className={({ isActive }) =>
                `group flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all ${
                  isActive
                    ? 'bg-white/10 text-white shadow-inner'
                    : 'text-slate-400 hover:bg-white/5 hover:text-white'
                }`
              }
            >
              {({ isActive }) => (
                <>
                  <span
                    className={`flex h-8 w-8 items-center justify-center rounded-lg transition ${
                      isActive ? 'bg-indigo-500/30 text-indigo-200' : 'bg-white/5 text-slate-400 group-hover:text-white'
                    }`}
                  >
                    <item.icon className="h-4 w-4" strokeWidth={2} />
                  </span>
                  {item.label}
                  {isActive && <span className="ml-auto h-1.5 w-1.5 rounded-full bg-indigo-400" />}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        <div className="border-t border-white/10 p-3">
          <button
            type="button"
            onClick={handleLogout}
            className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-slate-400 transition hover:bg-red-500/10 hover:text-red-300"
          >
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-white/5">
              <LogOut className="h-4 w-4" />
            </span>
            Sign out
          </button>
        </div>
      </aside>

      {/* Main */}
      <div className="ml-[260px] flex min-h-screen flex-1 flex-col">
        {/* Top bar */}
        <header className="sticky top-0 z-20 border-b border-slate-200/60 bg-white/70 px-8 py-4 backdrop-blur-xl">
          <div className="flex items-center justify-between gap-4">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-indigo-600">{role} Portal</p>
              <h2 className="text-lg font-bold text-slate-900">{currentPage?.label ?? 'Dashboard'}</h2>
            </div>
            <div className="flex items-center gap-3">
              <div className="hidden items-center gap-2 rounded-xl border border-slate-200/80 bg-slate-50/80 px-3 py-2 sm:flex">
                <Search className="h-4 w-4 text-slate-400" />
                <span className="text-sm text-slate-400">Search…</span>
              </div>
              <button type="button" className="rounded-xl border border-slate-200/80 bg-white p-2.5 text-slate-500 transition hover:bg-slate-50 hover:text-slate-700">
                <Bell className="h-4 w-4" />
              </button>
              <span className={`hidden rounded-xl bg-gradient-to-r px-3 py-1.5 text-xs font-bold text-white sm:inline ${roleColors[role]}`}>
                {role}
              </span>
            </div>
          </div>
        </header>

        <main className="flex-1 px-8 py-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
