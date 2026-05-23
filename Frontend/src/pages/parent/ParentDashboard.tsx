import { useEffect, useState } from 'react';
import { Users, CalendarCheck, CreditCard, BookOpen } from 'lucide-react';
import { Link } from 'react-router-dom';
import { msmsApi } from '../../api/msmsApi';
import { Card, CardBody, CardHeader, PageHeader, StatCard } from '../../components/ui';

export function ParentDashboard() {
  const [children, setChildren] = useState(0);

  useEffect(() => {
    msmsApi.parent.children().then((r) => setChildren(r.data.length));
  }, []);

  return (
    <>
      <PageHeader
        badge="Parent"
        title="Family Dashboard"
        description="Track your children's attendance, fees, and academic progress."
      />
      <section className="mb-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Linked children" value={children} icon={Users} theme="emerald" />
        <StatCard label="Attendance" value="View" icon={CalendarCheck} theme="sky" />
        <StatCard label="Payments" value="History" icon={CreditCard} theme="indigo" />
        <StatCard label="Results" value="Reports" icon={BookOpen} theme="violet" />
      </section>
      <Card>
        <CardHeader title="Quick links" />
        <CardBody className="grid gap-3 sm:grid-cols-2">
          <Link to="/parent/children" className="rounded-xl bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-800 transition hover:bg-emerald-100">
            My children
          </Link>
          <Link to="/parent/payments" className="rounded-xl bg-indigo-50 px-4 py-3 text-sm font-semibold text-indigo-800 transition hover:bg-indigo-100">
            Fee payments
          </Link>
        </CardBody>
      </Card>
    </>
  );
}
