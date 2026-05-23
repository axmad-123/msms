import { useEffect, useState, type FormEvent } from 'react';
import { CreditCard, DollarSign, Plus, Search, WalletCards } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { ClassItem, FinanceMonthSummary, MonthlyFee, Payment, StudentItem } from '../../types';
import { PAYMENT_STATUS } from '../../types';
import { matchesSearch } from '../../utils/search';
import {
  Alert, Badge, Button, Card, CardBody, CardHeader, EmptyState, Input, Modal,
  PageHeader, Select, StatCard, TableWrap, Tabs, Td, Th,
} from '../../components/ui';

type Tab = 'fees' | 'payments';

export function FinancePage() {
  const [tab, setTab] = useState<Tab>('payments');
  const [classes, setClasses] = useState<ClassItem[]>([]);
  const [students, setStudents] = useState<StudentItem[]>([]);
  const [fees, setFees] = useState<MonthlyFee[]>([]);
  const [payments, setPayments] = useState<Payment[]>([]);
  const [summary, setSummary] = useState<FinanceMonthSummary | null>(null);
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const [feeForm, setFeeForm] = useState({
    studentId: '', year: String(new Date().getFullYear()), month: '1', amount: '', academicYear: '2025-2026',
  });
  const [payForm, setPayForm] = useState({
    studentId: '', year: String(new Date().getFullYear()), month: '1', amount: '',
    paymentDate: new Date().toISOString().slice(0, 10), status: '1', notes: '',
  });
  const [selectedClassId, setSelectedClassId] = useState('');
  const [chargeStudentId, setChargeStudentId] = useState('');
  const [chargeSearch, setChargeSearch] = useState('');
  const [formStudentSearch, setFormStudentSearch] = useState('');
  const [chargeMsg, setChargeMsg] = useState('');
  const [charging, setCharging] = useState(false);

  const load = () => {
    msmsApi.finance.listFees().then((r) => setFees(r.data));
    msmsApi.finance.listPayments().then((r) => setPayments(r.data));
    msmsApi.finance.summary().then((r) => setSummary(r.data));
    msmsApi.people.listAllStudents().then((r) => setStudents(r.data.items));
    msmsApi.school.listClasses().then((r) => setClasses(r.data));
  };

  useEffect(() => { load(); }, []);

  const studentName = (id: string) => {
    const s = students.find((x) => x.id === id);
    return s ? `${s.firstName} ${s.lastName}` : id.slice(0, 8);
  };
  const studentMeta = (id: string) => students.find((x) => x.id === id);
  const selectedClass = classes.find((c) => c.id === selectedClassId);
  const classStudents = selectedClassId
    ? students.filter((s) => s.classId === selectedClassId)
    : [];
  const selectedChargeStudent = studentMeta(chargeStudentId);
  const filteredChargeStudents = classStudents.filter((s) =>
    matchesSearch(chargeSearch, [
      s.firstName,
      s.lastName,
      `${s.firstName} ${s.lastName}`,
      s.studentNumber,
      s.className,
      s.gradeLevel,
    ])
  );
  const filteredFormStudents = classStudents.filter((s) =>
    matchesSearch(formStudentSearch, [
      s.firstName,
      s.lastName,
      `${s.firstName} ${s.lastName}`,
      s.studentNumber,
      s.className,
      s.gradeLevel,
    ])
  );
  const filteredFees = fees.filter((f) => {
    const s = studentMeta(f.studentId);
    const classMatch = !selectedClassId || s?.classId === selectedClassId;
    return classMatch && matchesSearch(search, [
      studentName(f.studentId),
      s?.studentNumber,
      s?.className,
      f.month,
      f.year,
      f.amount,
      f.academicYear,
    ]);
  });
  const filteredPayments = payments.filter((p) => {
    const s = studentMeta(p.studentId);
    const classMatch = !selectedClassId || s?.classId === selectedClassId;
    return classMatch && matchesSearch(search, [
      studentName(p.studentId),
      s?.studentNumber,
      s?.className,
      p.paymentDate,
      p.amount,
      PAYMENT_STATUS[p.status],
      p.month,
      p.year,
      p.notes,
    ]);
  });
  const filteredSummaryStudents = (summary?.students ?? []).filter((s) => {
    const meta = studentMeta(s.studentId);
    const classMatch = !selectedClassId || meta?.classId === selectedClassId;
    return classMatch && matchesSearch(search, [
      s.studentName,
      s.studentNumber,
      s.className,
      s.feeAmount,
      s.paidAmount,
      s.balance,
      s.isPaid ? 'paid' : 'owes unpaid debt',
    ]);
  });
  const owedStudents = filteredSummaryStudents.filter((s) => !s.isPaid);
  const paidStudents = filteredSummaryStudents.filter((s) => s.isPaid);

  const chargeThisMonth = async () => {
    if (!chargeStudentId) return;
    setCharging(true);
    setChargeMsg('');
    setError('');
    const now = new Date();
    try {
      await msmsApi.finance.chargeMonth(chargeStudentId, now.getFullYear(), now.getMonth() + 1);
      setChargeMsg('Monthly fee charged successfully.');
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setCharging(false);
    }
  };

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      if (tab === 'fees') {
        await msmsApi.finance.upsertFee({
          studentId: feeForm.studentId,
          year: Number(feeForm.year),
          month: Number(feeForm.month),
          amount: Number(feeForm.amount),
          academicYear: feeForm.academicYear,
        });
      } else {
        await msmsApi.finance.createPayment({
          studentId: payForm.studentId,
          year: Number(payForm.year),
          month: Number(payForm.month),
          amount: Number(payForm.amount),
          paymentDate: payForm.paymentDate,
          status: Number(payForm.status),
          notes: payForm.notes || null,
        });
      }
      setOpen(false);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageHeader
        title="Finance"
        description="Who paid this month, who still owes, monthly fees and payment records"
        action={<Button onClick={() => setOpen(true)}><Plus className="h-4 w-4" /> Add</Button>}
      />

      {summary && (
        <div className="mb-6 grid gap-4 md:grid-cols-4">
          <StatCard label="Expected this month" value={`$${summary.expectedAmount}`} icon={DollarSign} theme="indigo" />
          <StatCard label="Paid this month" value={`$${summary.paidAmount}`} icon={WalletCards} theme="emerald" />
          <StatCard label="Outstanding" value={`$${summary.outstandingAmount}`} icon={CreditCard} theme="amber" />
          <StatCard label="Unpaid students" value={summary.unpaidStudents} icon={CreditCard} theme="rose" />
        </div>
      )}

      <Card className="mb-6 border-indigo-100 bg-gradient-to-br from-white to-indigo-50/40">
        <CardHeader title="1. Dooro fasalka, kadib ardayga lacagta ka jar" subtitle="Fasalka dooro marka hore. Ardayda fasalkaas kaliya ayaa kuu soo baxaysa." />
        <CardBody className="grid gap-3 lg:grid-cols-[1fr_1fr_1fr_auto] lg:items-end">
          <Select
            label="Class"
            value={selectedClassId}
            onChange={(e) => {
              setSelectedClassId(e.target.value);
              setChargeStudentId('');
              setChargeSearch('');
              setFormStudentSearch('');
            }}
          >
            <option value="">— Select class first —</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>{c.name} (Grade {c.gradeLevel})</option>
            ))}
          </Select>
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-slate-700">Search student or class</span>
            <span className="relative block">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
              <input
                value={chargeSearch}
                onChange={(e) => setChargeSearch(e.target.value)}
                placeholder={selectedClassId ? 'Type name or student ID...' : 'Select class first'}
                disabled={!selectedClassId}
                className="w-full rounded-xl border border-slate-200 bg-slate-50/80 py-2.5 pl-9 pr-3 text-sm outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10"
              />
            </span>
          </label>
          <Select
            label={`Student (${filteredChargeStudents.length} found)`}
            value={chargeStudentId}
            onChange={(e) => setChargeStudentId(e.target.value)}
            disabled={!selectedClassId}
          >
            <option value="">— Select student —</option>
            {filteredChargeStudents.map((s) => (
              <option key={s.id} value={s.id}>
                {s.firstName} {s.lastName} ({s.studentNumber}) — {s.className ?? 'No class'}
              </option>
            ))}
          </Select>
          <Button onClick={chargeThisMonth} loading={charging} disabled={!chargeStudentId}>
            <CreditCard className="h-4 w-4" /> Charge this month
          </Button>
          {selectedClass && (
            <div className="rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 lg:col-span-4">
              Class selected: <strong>{selectedClass.name}</strong>
              <span className="ml-2 text-slate-500">Students in class: {classStudents.length}</span>
            </div>
          )}
          {selectedChargeStudent && (
            <div className="rounded-xl border border-indigo-100 bg-indigo-50 px-4 py-3 text-sm text-indigo-800 lg:col-span-4">
              Selected: <strong>{selectedChargeStudent.firstName} {selectedChargeStudent.lastName}</strong>
              <span className="ml-2 text-indigo-600">Class: {selectedChargeStudent.className ?? 'No class'} · ID: {selectedChargeStudent.studentNumber}</span>
            </div>
          )}
        </CardBody>
        {chargeMsg && <CardBody className="pt-0"><Alert message={chargeMsg} type="success" /></CardBody>}
      </Card>

      <div className="mb-6">
        <Tabs
          tabs={[
            { id: 'fees' as Tab, label: 'Monthly fees' },
            { id: 'payments' as Tab, label: 'Payments' },
          ]}
          active={tab}
          onChange={setTab}
        />
      </div>

      <Card className="mb-6">
        <CardBody>
          <label className="relative block">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search student, class, paid, unpaid, month, amount..."
              className="w-full rounded-xl border border-slate-200 bg-slate-50/80 py-2.5 pl-9 pr-3 text-sm outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10"
            />
          </label>
        </CardBody>
      </Card>

      {tab === 'fees' ? (
        <Card>
          <CardHeader title="Monthly Fees" subtitle={`${filteredFees.length} of ${fees.length} records`} />
          <CardBody className="p-0">
            {filteredFees.length === 0 ? <EmptyState message="No fees." /> : (
              <TableWrap>
                <thead><tr><Th>Student</Th><Th>Class</Th><Th>Period</Th><Th>Amount</Th></tr></thead>
                <tbody>
                  {filteredFees.map((f) => (
                    <tr key={f.id}>
                      <Td>{studentName(f.studentId)}</Td>
                      <Td>{studentMeta(f.studentId)?.className ?? '—'}</Td>
                      <Td>{f.month}/{f.year}</Td>
                      <Td>${f.amount}</Td>
                    </tr>
                  ))}
                </tbody>
              </TableWrap>
            )}
          </CardBody>
        </Card>
      ) : (
        <Card>
          <CardHeader title="Payments" subtitle={`${filteredPayments.length} of ${payments.length} records`} />
          <CardBody className="p-0">
            {filteredPayments.length === 0 ? <EmptyState message="No payments." /> : (
              <TableWrap>
                <thead><tr><Th>Student</Th><Th>Class</Th><Th>Date</Th><Th>Amount</Th><Th>Status</Th></tr></thead>
                <tbody>
                  {filteredPayments.map((p) => (
                    <tr key={p.id}>
                      <Td>{studentName(p.studentId)}</Td>
                      <Td>{studentMeta(p.studentId)?.className ?? '—'}</Td>
                      <Td>{p.paymentDate}</Td>
                      <Td>${p.amount}</Td>
                      <Td><Badge tone={p.status === 1 ? 'success' : 'default'}>{PAYMENT_STATUS[p.status]}</Badge></Td>
                    </tr>
                  ))}
                </tbody>
              </TableWrap>
            )}
          </CardBody>
        </Card>
      )}

      {summary && (
        <div className="mt-6 grid gap-6 xl:grid-cols-2">
          <Card className="border-amber-200">
            <CardHeader title="Ardayda Lacag Lagu Leeyahay" subtitle={`${owedStudents.length} students owe money${selectedClass ? ` · ${selectedClass.name}` : ''}`} />
            <CardBody className="p-0">
              {owedStudents.length === 0 ? (
                <EmptyState message="No unpaid students for this filter." />
              ) : (
                <TableWrap>
                  <thead><tr><Th>Student</Th><Th>Class</Th><Th>Fee</Th><Th>Paid</Th><Th>Balance</Th></tr></thead>
                  <tbody>
                    {owedStudents.map((s) => (
                      <tr key={s.studentId}>
                        <Td className="font-medium">{s.studentName}<span className="block text-xs text-slate-400">{s.studentNumber}</span></Td>
                        <Td>{s.className ?? '—'}</Td>
                        <Td>${s.feeAmount}</Td>
                        <Td>${s.paidAmount}</Td>
                        <Td><Badge tone="warning">${s.balance}</Badge></Td>
                      </tr>
                    ))}
                  </tbody>
                </TableWrap>
              )}
            </CardBody>
          </Card>

          <Card className="border-emerald-200">
            <CardHeader title="Ardayda Lacagta Bixisay" subtitle={`${paidStudents.length} students paid${selectedClass ? ` · ${selectedClass.name}` : ''}`} />
            <CardBody className="p-0">
              {paidStudents.length === 0 ? (
                <EmptyState message="No paid students for this filter." />
              ) : (
                <TableWrap>
                  <thead><tr><Th>Student</Th><Th>Class</Th><Th>Fee</Th><Th>Paid</Th><Th>Status</Th></tr></thead>
                  <tbody>
                    {paidStudents.map((s) => (
                      <tr key={s.studentId}>
                        <Td className="font-medium">{s.studentName}<span className="block text-xs text-slate-400">{s.studentNumber}</span></Td>
                        <Td>{s.className ?? '—'}</Td>
                        <Td>${s.feeAmount}</Td>
                        <Td>${s.paidAmount}</Td>
                        <Td><Badge tone="success">Paid</Badge></Td>
                      </tr>
                    ))}
                  </tbody>
                </TableWrap>
              )}
            </CardBody>
          </Card>
        </div>
      )}

      <Modal open={open} onClose={() => setOpen(false)} title={tab === 'fees' ? 'Set Monthly Fee' : 'Record Payment'}>
        {error && <Alert message={error} />}
        <form onSubmit={submit} className="space-y-3">
          <Select
            label="Class"
            value={selectedClassId}
            onChange={(e) => {
              setSelectedClassId(e.target.value);
              setFormStudentSearch('');
              setFeeForm({ ...feeForm, studentId: '' });
              setPayForm({ ...payForm, studentId: '' });
            }}
          >
            <option value="">— Select class first —</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>{c.name} (Grade {c.gradeLevel})</option>
            ))}
          </Select>
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-slate-700">Search student or class</span>
            <span className="relative block">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
              <input
                value={formStudentSearch}
                onChange={(e) => setFormStudentSearch(e.target.value)}
                placeholder={selectedClassId ? 'Type name or student ID...' : 'Select class first'}
                disabled={!selectedClassId}
                className="w-full rounded-xl border border-slate-200 bg-slate-50/80 py-2.5 pl-9 pr-3 text-sm outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10"
              />
            </span>
          </label>
          <Select
            label={`Student (${filteredFormStudents.length} found)`}
            value={tab === 'fees' ? feeForm.studentId : payForm.studentId}
            onChange={(e) =>
              tab === 'fees'
                ? setFeeForm({ ...feeForm, studentId: e.target.value })
                : setPayForm({ ...payForm, studentId: e.target.value })
            }
            disabled={!selectedClassId}
            required
          >
            <option value="">— Select —</option>
            {filteredFormStudents.map((s) => (
              <option key={s.id} value={s.id}>
                {s.firstName} {s.lastName} ({s.studentNumber}) — {s.className ?? 'No class'}
              </option>
            ))}
          </Select>
          {tab === 'fees' ? (
            <>
              <Input label="Year" type="number" value={feeForm.year} onChange={(e) => setFeeForm({ ...feeForm, year: e.target.value })} />
              <Input label="Month" type="number" min={1} max={12} value={feeForm.month} onChange={(e) => setFeeForm({ ...feeForm, month: e.target.value })} />
              <Input label="Amount" type="number" value={feeForm.amount} onChange={(e) => setFeeForm({ ...feeForm, amount: e.target.value })} required />
              <Input label="Academic Year" value={feeForm.academicYear} onChange={(e) => setFeeForm({ ...feeForm, academicYear: e.target.value })} />
            </>
          ) : (
            <>
              <Input label="Year" type="number" value={payForm.year} onChange={(e) => setPayForm({ ...payForm, year: e.target.value })} />
              <Input label="Month" type="number" min={1} max={12} value={payForm.month} onChange={(e) => setPayForm({ ...payForm, month: e.target.value })} />
              <Input label="Amount" type="number" value={payForm.amount} onChange={(e) => setPayForm({ ...payForm, amount: e.target.value })} required />
              <Input label="Payment Date" type="date" value={payForm.paymentDate} onChange={(e) => setPayForm({ ...payForm, paymentDate: e.target.value })} />
              <Select label="Status" value={payForm.status} onChange={(e) => setPayForm({ ...payForm, status: e.target.value })}>
                {Object.entries(PAYMENT_STATUS).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </Select>
              <Input label="Notes" value={payForm.notes} onChange={(e) => setPayForm({ ...payForm, notes: e.target.value })} />
            </>
          )}
          <span className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setOpen(false)}>Cancel</Button>
            <Button type="submit" loading={loading}>Save</Button>
          </span>
        </form>
      </Modal>
    </>
  );
}
