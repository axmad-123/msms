import { useEffect, useState } from 'react';
import { School, CalendarCheck, ClipboardList, BookOpen } from 'lucide-react';
import { Link } from 'react-router-dom';
import { msmsApi } from '../../api/msmsApi';
import { Card, CardBody, CardHeader, PageHeader, StatCard } from '../../components/ui';

export function TeacherDashboard() {
  const [count, setCount] = useState(0);

  useEffect(() => {
    msmsApi.teacher.assignments().then((r) => setCount(r.data.length));
  }, []);

  return (
    <>
      <PageHeader
        badge="Teacher"
        title="My Dashboard"
        description="Your assigned classes, attendance tools, and grade entry."
      />
      <section className="mb-8 grid gap-5 sm:grid-cols-3">
        <StatCard label="Class assignments" value={count} icon={School} theme="indigo" />
        <StatCard label="Attendance" value="Ready" icon={CalendarCheck} theme="sky" />
        <StatCard label="Grade entry" value="Open" icon={ClipboardList} theme="violet" />
      </section>
      <Card>
        <CardHeader title="Teaching tools" subtitle="Jump to your daily tasks" />
        <CardBody className="grid gap-3 sm:grid-cols-3">
          <Link to="/teacher/assignments" className="flex items-center gap-3 rounded-xl border border-slate-100 bg-slate-50/80 p-4 transition hover:border-indigo-200 hover:bg-indigo-50/50">
            <BookOpen className="h-5 w-5 text-indigo-600" />
            <span className="text-sm font-semibold text-slate-800">View classes</span>
          </Link>
          <Link to="/teacher/attendance" className="flex items-center gap-3 rounded-xl border border-slate-100 bg-slate-50/80 p-4 transition hover:border-sky-200 hover:bg-sky-50/50">
            <CalendarCheck className="h-5 w-5 text-sky-600" />
            <span className="text-sm font-semibold text-slate-800">Mark attendance</span>
          </Link>
          <Link to="/teacher/exams" className="flex items-center gap-3 rounded-xl border border-slate-100 bg-slate-50/80 p-4 transition hover:border-violet-200 hover:bg-violet-50/50">
            <ClipboardList className="h-5 w-5 text-violet-600" />
            <span className="text-sm font-semibold text-slate-800">Enter marks</span>
          </Link>
        </CardBody>
      </Card>
    </>
  );
}
