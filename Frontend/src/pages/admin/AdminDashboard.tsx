import { useEffect, useState } from 'react';
import { Users, School, GraduationCap, CreditCard, TrendingUp, Calendar } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import { Card, CardBody, CardHeader, PageHeader, StatCard } from '../../components/ui';

export function AdminDashboard() {
  const [stats, setStats] = useState({ students: 0, classes: 0, teachers: 0, payments: 0 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      msmsApi.people.listAllStudents(),
      msmsApi.school.listClasses(),
      msmsApi.people.listTeachers(),
      msmsApi.finance.listPayments(),
    ])
      .then(([s, c, t, p]) => {
        setStats({ students: s.data.totalCount, classes: c.data.length, teachers: t.data.length, payments: p.data.length });
      })
      .finally(() => setLoading(false));
  }, []);

  const today = new Date().toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });

  return (
    <>
      <PageHeader
        badge="Overview"
        title="Admin Dashboard"
        description="Monitor enrollment, academics, and school finances at a glance."
      />

      <Card className="mb-8 border-indigo-100 bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-500/20">
        <CardBody className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="text-sm font-medium text-indigo-100">Good day, Administrator</p>
            <p className="mt-1 text-xl font-bold">{today}</p>
          </div>
          <div className="flex items-center gap-2 rounded-xl bg-white/15 px-4 py-2 text-sm font-medium backdrop-blur">
            <TrendingUp className="h-4 w-4" />
            System operational
          </div>
        </CardBody>
      </Card>

      <section className="mb-8 grid gap-5 sm:grid-cols-2 xl:grid-cols-4">
        {loading ? (
          Array.from({ length: 4 }).map((_, i) => (
            <Card key={i}><CardBody><div className="skeleton h-20 rounded-xl" /></CardBody></Card>
          ))
        ) : (
          <>
            <StatCard label="Total Students" value={stats.students} icon={Users} theme="indigo" trend="Active enrollment" />
            <StatCard label="Classes" value={stats.classes} icon={School} theme="sky" />
            <StatCard label="Teachers" value={stats.teachers} icon={GraduationCap} theme="violet" />
            <StatCard label="Payments" value={stats.payments} icon={CreditCard} theme="emerald" trend="Recorded transactions" />
          </>
        )}
      </section>

      <section className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader title="Quick actions" subtitle="Common administrative tasks" />
          <CardBody className="grid gap-3 sm:grid-cols-2">
            {[
              { label: 'Add student', href: '/admin/students', color: 'bg-indigo-50 text-indigo-700 hover:bg-indigo-100' },
              { label: 'Take attendance', href: '/admin/attendance', color: 'bg-sky-50 text-sky-700 hover:bg-sky-100' },
              { label: 'Record payment', href: '/admin/finance', color: 'bg-emerald-50 text-emerald-700 hover:bg-emerald-100' },
              { label: 'Exam catalog', href: '/admin/exams', color: 'bg-violet-50 text-violet-700 hover:bg-violet-100' },
            ].map((a) => (
              <a
                key={a.href}
                href={a.href}
                className={`rounded-xl px-4 py-3 text-sm font-semibold transition ${a.color}`}
              >
                {a.label}
              </a>
            ))}
          </CardBody>
        </Card>
        <Card>
          <CardHeader title="Academic year" subtitle="2025–2026" />
          <CardBody>
            <div className="flex items-center gap-4 rounded-xl border border-slate-100 bg-slate-50/80 p-4">
              <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
                <Calendar className="h-6 w-6" />
              </span>
              <div>
                <p className="font-semibold text-slate-900">Current term in progress</p>
                <p className="mt-0.5 text-sm text-slate-500">Manage classes, exams, and promotions from the sidebar.</p>
              </div>
            </div>
          </CardBody>
        </Card>
      </section>
    </>
  );
}
